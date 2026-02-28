#Requires -Version 5.1
<#
.SYNOPSIS
  FastWhisper localhost install wizard.
  Creates .venv, installs PyTorch (CUDA 12.4), Flash Attention wheel, and API deps.
#>

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# --- Config (matches requirements-api.txt and wheel) ---
$PythonMinVersion = "3.11"
$RepoRoot       = Resolve-Path (Join-Path $PSScriptRoot "..\..\..")
$VenvPath       = Join-Path $RepoRoot ".venv"
$WheelUrl       = "https://huggingface.co/lldacing/flash-attention-windows-wheel/resolve/main/flash_attn-2.7.4%2Bcu124torch2.6.0cxx11abiFALSE-cp311-cp311-win_amd64.whl?download=true"
$WheelFileName  = "flash_attn-2.7.4+cu124torch2.6.0cxx11abiFALSE-cp311-cp311-win_amd64.whl"
$WheelPath      = Join-Path $RepoRoot $WheelFileName
$Requirements   = Join-Path $RepoRoot "apps\api\requirements-api.txt"
$PyTorchIndex   = "https://download.pytorch.org/whl/cu124"

# --- Helpers ---
function Write-Step { param([string]$Msg) Write-Host "`n[*] $Msg" -ForegroundColor Cyan }
function Write-Ok   { param([string]$Msg) Write-Host "    $Msg" -ForegroundColor Green }
function Write-Warn { param([string]$Msg) Write-Host "    $Msg" -ForegroundColor Yellow }
function Write-Err  { param([string]$Msg) Write-Host "    $Msg" -ForegroundColor Red   }

function Get-PythonExe {
    foreach ($v in @("3.11", "3.12", "3")) {
        try {
            $out = & py -$v -c "import sys; print(sys.executable)" 2>$null
            if ($LASTEXITCODE -eq 0 -and $out) { return $out.Trim() }
        } catch {}
    }
    try {
        $out = & python -c "import sys; print(sys.executable)" 2>$null
        if ($LASTEXITCODE -eq 0 -and $out) { return $out.Trim() }
    } catch {}
    $null
}

function Test-PythonVersion {
    param([string]$Exe)
    $ver = & $Exe -c "import sys; v=sys.version_info; print(f'{v.major}.{v.minor}')" 2>$null
    if (-not $ver) { return $false }
    $major, $minor = $ver.Trim().Split(".")
    $major -ge 3 -and [int]$minor -ge 11
}

# --- Main wizard ---
Write-Host ""
Write-Host "  =============================================" -ForegroundColor Magenta
Write-Host "    FastWhisper – Localhost Install Wizard"     -ForegroundColor Magenta
Write-Host "  =============================================" -ForegroundColor Magenta

# 1. Python
Write-Step "Checking Python $PythonMinVersion..."
$pythonExe = Get-PythonExe
if (-not $pythonExe) {
    Write-Err "Python $PythonMinVersion not found. Install from https://www.python.org/ and ensure 'py' or 'python' is on PATH."
    exit 1
}
if (-not (Test-PythonVersion -Exe $pythonExe)) {
    Write-Err "Need Python $PythonMinVersion or newer. Found: $(& $pythonExe --version 2>&1)"
    exit 1
}
Write-Ok "Using: $pythonExe"

# 2. Venv
Write-Step "Setting up virtual environment at .venv..."
if (Test-Path $VenvPath) {
    Write-Warn ".venv already exists. Reuse? (Y/n)"
    $r = Read-Host
    if ($r -match '^n') {
        Remove-Item -Recurse -Force $VenvPath
        & $pythonExe -m venv $VenvPath
        Write-Ok "Created new .venv"
    } else {
        Write-Ok "Reusing existing .venv"
    }
} else {
    & $pythonExe -m venv $VenvPath
    Write-Ok "Created .venv"
}

$pip = Join-Path $VenvPath "Scripts\pip.exe"
$pythonVenv = Join-Path $VenvPath "Scripts\python.exe"
if (-not (Test-Path $pip)) {
    Write-Err "pip not found in venv: $pip"
    exit 1
}

# 3. Upgrade pip
Write-Step "Upgrading pip..."
& $pip install --upgrade pip -q
Write-Ok "pip upgraded"

# 4. PyTorch CUDA 12.4
Write-Step "Installing PyTorch 2.6 + CUDA 12.4..."
& $pip install torch==2.6.* torchvision torchaudio --index-url $PyTorchIndex -q
Write-Ok "PyTorch installed"

# 5. Flash Attention wheel
Write-Step "Flash Attention 2 (Windows wheel)..."
if (Test-Path $WheelPath) {
    Write-Ok "Wheel already present: $WheelFileName"
} else {
    Write-Host "    Downloading from Hugging Face..." -ForegroundColor Gray
    try {
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        Invoke-WebRequest -Uri $WheelUrl -OutFile $WheelPath -UseBasicParsing
        Write-Ok "Downloaded: $WheelFileName"
    } catch {
        Write-Err "Download failed: $_"
        exit 1
    }
}
& $pip install $WheelPath -q
Write-Ok "Flash Attention installed"

# 6. API requirements (no torch/flash in file – already installed)
Write-Step "Installing API dependencies (transformers, fastapi, etc.)..."
if (-not (Test-Path $Requirements)) {
    Write-Warn "requirements-api.txt not found; skipping."
} else {
    & $pip install -r $Requirements -q
    Write-Ok "Requirements installed"
}

# 7. Verify
Write-Step "Verifying install..."
$torchOk = & $pythonVenv -c "import torch; print(torch.cuda.is_available())" 2>$null
$flashOk = & $pythonVenv -c "import flash_attn; print('ok')" 2>$null
if ($torchOk -match "True") { Write-Ok "PyTorch CUDA: available" } else { Write-Warn "PyTorch CUDA: not available (CPU only)" }
if ($flashOk -match "ok")   { Write-Ok "Flash Attention: OK" } else { Write-Warn "Flash Attention: import failed" }

Write-Host ""
Write-Host "  ---------------------------------------------" -ForegroundColor Green
Write-Host "    Install complete."                          -ForegroundColor Green
Write-Host "  ---------------------------------------------" -ForegroundColor Green
Write-Host ""
Write-Host "  Activate venv:" -ForegroundColor White
Write-Host "    .\.venv\Scripts\Activate.ps1"               -ForegroundColor Gray
Write-Host ""
Write-Host "  Run API (if you have a start script):"        -ForegroundColor White
Write-Host "    uvicorn app.main:app --reload --host 127.0.0.1 --port 8000" -ForegroundColor Gray
Write-Host ""

$run = Read-Host "Activate .venv in this session now? (Y/n)"
if ($run -notmatch '^n') {
    & (Join-Path $VenvPath "Scripts\Activate.ps1")
    Write-Ok "Virtual environment activated. You can run your app from this shell."
}
