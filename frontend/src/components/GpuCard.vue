<template>
  <section class="card">
    <h2>GPU</h2>
    <p class="model" :title="gpu.name">{{ gpu.name }}</p>
    <div class="gauge-wrap" :class="tempCls">
      <CircularGauge :value="gpu.usage" :label="formatTemp(gpu.temperature)" :color="tempColor" />
    </div>
    <div class="usage-label" :class="usageCls">{{ formatPercent(gpu.usage) }}</div>
    <div v-if="showClock" class="clock">
      {{ formatClock(gpu.coreClock) }}
      <span v-if="gpu.memoryClock != null"> · MEM {{ formatClock(gpu.memoryClock) }}</span>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { AppSettings, GpuStatus } from '../models/hardware'
import CircularGauge from './CircularGauge.vue'
import { formatClock, formatPercent, formatTemp, tempLevel, usageLevel } from '../services/format'

const props = defineProps<{
  gpu: GpuStatus
  settings: AppSettings
  showClock: boolean
}>()

const tempCls = computed(() => `temp-${tempLevel(props.gpu.temperature, props.settings)}`)
const usageCls = computed(() => `usage-${usageLevel(props.gpu.usage, props.settings)}`)
const tempColor = computed(() => {
  const level = tempLevel(props.gpu.temperature, props.settings)
  if (level === 'crit') return '#ff7a7a'
  if (level === 'alert') return '#f0a36b'
  if (level === 'warn') return '#e8d27a'
  return '#7dffa3'
})
</script>
