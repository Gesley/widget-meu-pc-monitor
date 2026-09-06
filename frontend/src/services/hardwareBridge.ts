type MessageHandler = (payload: unknown) => void

class HardwareBridge {
  private handlers = new Map<string, Set<MessageHandler>>()
  private listener = (event: MessageEvent) => {
    const data = event.data as { type?: string; payload?: unknown }
    if (!data?.type) return
    this.handlers.get(data.type)?.forEach((handler) => handler(data.payload))
  }

  constructor() {
    window.chrome?.webview?.addEventListener('message', this.listener)
  }

  on(type: string, handler: MessageHandler) {
    if (!this.handlers.has(type)) this.handlers.set(type, new Set())
    this.handlers.get(type)!.add(handler)
    return () => this.handlers.get(type)?.delete(handler)
  }

  send(type: string, payload?: unknown) {
    window.chrome?.webview?.postMessage({ type, payload })
  }

  ready() {
    this.send('ready')
  }

  drag() {
    this.send('drag')
  }
}

export const hardwareBridge = new HardwareBridge()
