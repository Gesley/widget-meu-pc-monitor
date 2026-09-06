export interface CpuStatus {
  name: string
  temperature: number | null
  usage: number | null
  clock: number | null
  temperatureHistory: Array<number | null>
  usageHistory: Array<number | null>
}

export interface GpuStatus {
  name: string
  identifier: string | null
  temperature: number | null
  usage: number | null
  coreClock: number | null
  memoryClock: number | null
  temperatureHistory: Array<number | null>
  usageHistory: Array<number | null>
}

export interface RamStatus {
  used: number | null
  available: number | null
  total: number | null
  usage: number | null
  temperature: number | null
  usageHistory: Array<number | null>
}

export interface HardwareStatus {
  cpu: CpuStatus
  gpu: GpuStatus
  ram: RamStatus
  timestamp: string
}

export interface GpuOption {
  identifier: string
  name: string
}

export interface AppSettings {
  opacity: number
  alwaysOnTop: boolean
  startWithWindows: boolean
  showCharts: boolean
  showClock: boolean
  showRam: boolean
  pollIntervalMs: number
  layout: 'compact' | 'normal'
  theme: 'dark' | 'light' | 'amoled'
  interactionMode: boolean
  selectedGpuId: string | null
  tempWarn: number
  tempAlert: number
  tempCritical: number
  usageWarn: number
  usageHigh: number
}

export const defaultHardware: HardwareStatus = {
  cpu: {
    name: 'CPU',
    temperature: null,
    usage: null,
    clock: null,
    temperatureHistory: [],
    usageHistory: []
  },
  gpu: {
    name: 'GPU',
    identifier: null,
    temperature: null,
    usage: null,
    coreClock: null,
    memoryClock: null,
    temperatureHistory: [],
    usageHistory: []
  },
  ram: {
    used: null,
    available: null,
    total: null,
    usage: null,
    temperature: null,
    usageHistory: []
  },
  timestamp: new Date().toISOString()
}

export const defaultSettings: AppSettings = {
  opacity: 0.92,
  alwaysOnTop: true,
  startWithWindows: false,
  showCharts: true,
  showClock: true,
  showRam: true,
  pollIntervalMs: 1000,
  layout: 'normal',
  theme: 'dark',
  interactionMode: true,
  selectedGpuId: null,
  tempWarn: 60,
  tempAlert: 80,
  tempCritical: 90,
  usageWarn: 50,
  usageHigh: 80
}
