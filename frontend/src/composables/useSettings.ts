import { onMounted, reactive } from 'vue'
import { defaultSettings, type AppSettings } from '../models/hardware'
import { hardwareBridge } from '../services/hardwareBridge'

export function useSettings() {
  const settings = reactive<AppSettings>({ ...defaultSettings })

  onMounted(() => {
    hardwareBridge.on('settings', (payload) => {
      Object.assign(settings, payload)
    })
  })

  function persist() {
    hardwareBridge.send('settings', { ...settings })
  }

  return { settings, persist }
}
