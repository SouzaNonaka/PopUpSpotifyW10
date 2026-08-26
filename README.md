# Spotify Media Flyout — Windows 11

Reprodução nativa, leve e moderna em **C# + .NET + WPF** do clássico flyout de volume e mídia do Windows 10 para Windows 11.

---

## Recursos da V1

- **Posicionamento Fixo no Canto Superior Esquerdo**: Posicionado exatamente em `WorkArea.Left + 50px` e `WorkArea.Top + 50px` no monitor principal, com suporte nativo a Per-Monitor DPI (100%, 125%, 150%, 200%).
- **Controle de Volume do Windows**:
  - Integração direta de baixo nível via WASAPI CoreAudio (`IAudioEndpointVolume`).
  - Notificações de eventos em tempo real para teclas de volume do teclado e alterações externas (zero polling, 0% CPU idle).
  - Ícone de alto-falante dinâmico (Mudo, Baixo, Médio, Alto) com clique para Mute/Unmute.
  - Barra de volume vertical e ajuste via roda do mouse (`MouseWheel`).
- **Integração com Spotify (GSMTC)**:
  - Detecta a sessão ativa do Spotify via Windows *Global System Media Transport Controls*.
  - Exibe título da música, artista e capa do álbum em tempo real.
  - Botões de controle de reprodução nativos: **Anterior (◀)**, **Play / Pause (❚❚ / ▶)** e **Próxima (▶)**.
- **Janela Não Intrusiva**:
  - Configurada com `WS_EX_NOACTIVATE` e `WS_EX_TOOLWINDOW` para **nunca roubar o foco** de jogos, navegadores ou editores.
  - Animação suave de fade (180ms) e timer automático de 2 segundos.
  - Pausa o timer automaticamente quando o cursor do mouse estiver sobre o flyout.
- **System Tray & Inicialização**:
  - Ícone na bandeja do sistema com menu de contexto:
    - `Spotify Media Flyout`
    - `☑ Iniciar com Windows` (configura a chave de registro `HKCU\...\Run` sem precisar de privilégios de administrador)
    - `Mostrar teste` (exibe o flyout imediatamente para testes)
    - `Sair`

---

## Como Executar e Compilar

### Requisitos
- Windows 10 (Build 19041+) ou Windows 11
- .NET 8.0 SDK / .NET 9.0 SDK / .NET 10.0 SDK

### Compilação e Execução
```powershell
# Restaurar e compilar em Release
dotnet build -c Release

# Executar o aplicativo em background
dotnet run -c Release
```

O executável final estará disponível em:
`bin\Release\net8.0-windows10.0.22621.0\SpotifyMediaFlyout.exe`
