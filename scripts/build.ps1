$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

Push-Location "$root\frontend"
npm install
npm run build
Pop-Location

dotnet restore "$root\PCMonitor.sln"
dotnet build "$root\PCMonitor.sln" -c Release
Write-Host "Build concluído. Executável em src\PCMonitor\bin\Release\PCMonitor.exe"
