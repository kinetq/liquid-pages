param([string]$ServerPath)

$serverPath = Resolve-Path $ServerPath
Write-Host "Starting $serverPath" -ForegroundColor Cyan

$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = $serverPath
$psi.RedirectStandardInput = $true
$psi.RedirectStandardOutput = $true
$psi.RedirectStandardError = $true
$psi.UseShellExecute = $false
$psi.CreateNoWindow = $true

$p = [System.Diagnostics.Process]::Start($psi)
Start-Sleep -Milliseconds 200

# Send initialize request with framing
$req = @{jsonrpc="2.0";id=1;method="initialize";params=@{processId=$pid;rootUri=$null;capabilities=@{}}} | ConvertTo-Json -Compress
$bytes = [System.Text.Encoding]::UTF8.GetBytes($req)
$header = "Content-Length: $($bytes.Length)`r`n`r`n"
$p.StandardInput.Write($header)
$p.StandardInput.Write($req)
$p.StandardInput.Flush()

# Wait for response with timeout (using async read with timeout)
$readTask = $p.StandardOutput.ReadLineAsync()
if ($readTask.Wait(5000)) {
    $line = $readTask.Result
    Write-Host "Response line: $line" -ForegroundColor Green
} else {
    Write-Host "Timeout - no response" -ForegroundColor Red
}

# Show stderr
Start-Sleep -Milliseconds 200
$stderr = $p.StandardError.ReadToEnd()
if ($stderr) { Write-Host "Stderr: $stderr" -ForegroundColor Magenta }

$p.Kill()