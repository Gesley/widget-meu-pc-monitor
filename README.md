# PC Monitor

Widget HUD flutuante para Windows que monitora CPU, GPU e RAM em tempo real.

A interface foi inspirada em overlays de hardware (gauges circulares, hierarquia de temperatura/uso e gráficos compactos), sem copiar um layout existente.

## Objetivo

Exibir, de forma leve e permanente na área de trabalho:

- CPU: modelo, temperatura, uso e clock (se o sensor existir)
- GPU: modelo, temperatura, uso e clocks (se existirem)
- RAM: uso, memória usada/total e temperatura somente se o hardware expuser esse sensor

FPS não faz parte do produto.

## Arquitetura

```
WPF (.NET 8) + WebView2
    ├── LibreHardwareMonitorLib
    ├── HardwareMonitorService
    └── WebViewBridge (JSON)
              └── Vue 3 + TypeScript + Vite
```

Não usa Electron. A UI Vue é empacotada em `src/PCMonitor/wwwroot` e carregada no WebView2 via host virtual `https://app.pcmonitor/`.

## Tecnologias

- C# / .NET 8 / WPF
- Microsoft WebView2
- LibreHardwareMonitorLib
- Vue 3, TypeScript, Vite

## Pré-requisitos

- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Node.js 18+
- Runtime do [WebView2](https://developer.microsoft.com/microsoft-edge/webview2/) (já presente na maioria das instalações do Windows 11)

## Desenvolvimento

### Frontend

```powershell
cd frontend
npm install
npm run dev
```

O modo `dev` do Vite serve a UI em `http://localhost:5173` para iterar no layout. A aplicação WPF usa o build estático, não o servidor Vite.

### Build da interface para o WPF

```powershell
cd frontend
npm install
npm run build
```

O Vite gera os arquivos em `src/PCMonitor/wwwroot`.

### Backend / aplicativo Windows

```powershell
dotnet restore PCMonitor.sln
dotnet build PCMonitor.sln -c Debug
dotnet run --project src/PCMonitor/PCMonitor.csproj
```

Release:

```powershell
dotnet build PCMonitor.sln -c Release
```

O executável fica em `src/PCMonitor/bin/Release/PCMonitor.exe`.

Script único (frontend + Release):

```powershell
.\scripts\build.ps1
```

## Sensores e limitações do LibreHardwareMonitor

- Nem todo hardware expõe temperatura de RAM, clock ou GPU package. Nesses casos a UI mostra `N/A`.
- Alguns sensores (especialmente temperaturas de CPU/placa) exigem o driver WinRing0 e, em alguns PCs, execução elevada.
- Nomes de sensores variam por fabricante. A descoberta prioriza tokens como `Package`, `Total`, `GPU Core`, sem depender de um modelo específico.
- Intel, AMD e NVIDIA são suportados. Se houver várias GPUs, a dedicada é escolhida por padrão; a GPU pode ser trocada nas configurações.
- Sem GPU dedicada, a integrada é usada se o LibreHardwareMonitor a listar.

## Como adicionar um novo sensor

1. Localize o hardware em `SensorDiscoveryService`.
2. Use `Pick` com tokens de nome e `SensorType` adequados.
3. Propague o valor numérico (ou `null`) no modelo `HardwareStatus`.
4. Formate no Vue; não formate no backend.

## Persistência

Configurações e posição da janela: `%AppData%\PCMonitor\settings.json`  
Log técnico: `%AppData%\PCMonitor\pcmonitor.log`

## Instalador

O MVP gera `PCMonitor.exe`. Um instalador (`PC Monitor Setup.exe`) pode ser adicionado depois (por exemplo com WiX ou Inno Setup) copiando a pasta de publish:

```powershell
dotnet publish src/PCMonitor/PCMonitor.csproj -c Release -r win-x64 --self-contained false
```
