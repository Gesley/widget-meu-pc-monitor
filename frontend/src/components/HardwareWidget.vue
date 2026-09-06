<template>
  <div class="hud" :class="settings.layout" :data-theme="settings.theme" @contextmenu.prevent="open = true">
    <div class="cards">
      <CpuCard :cpu="status.cpu" :settings="settings" :show-clock="settings.showClock" />
      <GpuCard :gpu="status.gpu" :settings="settings" :show-clock="settings.showClock" />
    </div>
    <div v-if="settings.showCharts" class="charts">
      <div class="chart-box">
        <h3>CPU TEMP</h3>
        <MiniChart :values="status.cpu.temperatureHistory" :max="100" />
      </div>
      <div class="chart-box">
        <h3>GPU TEMP</h3>
        <MiniChart :values="status.gpu.temperatureHistory" :max="100" />
      </div>
    </div>
    <RamCard v-if="settings.showRam" :ram="status.ram" :settings="settings" />
    <SettingsPanel
      v-if="open"
      :settings="settings"
      :gpus="gpus"
      @close="open = false"
      @persist="persist"
    />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useHardwareMonitor } from '../composables/useHardwareMonitor'
import { useSettings } from '../composables/useSettings'
import { hardwareBridge } from '../services/hardwareBridge'
import CpuCard from './CpuCard.vue'
import GpuCard from './GpuCard.vue'
import RamCard from './RamCard.vue'
import MiniChart from './MiniChart.vue'
import SettingsPanel from './SettingsPanel.vue'

const { status, gpus } = useHardwareMonitor()
const { settings, persist } = useSettings()
const open = ref(false)

hardwareBridge.on('openSettings', () => {
  open.value = true
})
</script>
