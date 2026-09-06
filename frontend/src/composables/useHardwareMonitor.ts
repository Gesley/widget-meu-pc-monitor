import { onMounted, onUnmounted, ref, shallowRef } from 'vue'
import { defaultHardware, type GpuOption, type HardwareStatus } from '../models/hardware'
import { hardwareBridge } from '../services/hardwareBridge'

export function useHardwareMonitor() {
  const status = shallowRef<HardwareStatus>(defaultHardware)
  const gpus = ref<GpuOption[]>([])

  onMounted(() => {
    const offHw = hardwareBridge.on('hardware', (payload) => {
      status.value = payload as HardwareStatus
    })
    const offGpu = hardwareBridge.on('gpus', (payload) => {
      gpus.value = payload as GpuOption[]
    })
    hardwareBridge.ready()
    onUnmounted(() => {
      offHw()
      offGpu()
    })
  })

  return { status, gpus }
}
