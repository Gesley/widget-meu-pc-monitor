declare module '*.vue' {
  import type { DefineComponent } from 'vue'
  const component: DefineComponent<object, object, unknown>
  export default component
}

interface ChromeWebView {
  postMessage: (message: unknown) => void
  addEventListener: (type: 'message', handler: (event: MessageEvent) => void) => void
  removeEventListener: (type: 'message', handler: (event: MessageEvent) => void) => void
}

interface Window {
  chrome?: {
    webview?: ChromeWebView
  }
}
