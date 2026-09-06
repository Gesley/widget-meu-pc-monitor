<template>
  <section class="card">
    <h2>CPU</h2>
    <p class="model" :title="cpu.name">{{ cpu.name }}</p>
    <div class="gauge-wrap" :class="tempCls">
      <CircularGauge :value="cpu.usage" :label="formatTemp(cpu.temperature)" :color="tempColor" />
    </div>
    <div class="usage-label" :class="usageCls">{{ formatPercent(cpu.usage) }}</div>
    <div v-if="showClock" class="clock">{{ formatClock(cpu.clock) }}</div>
  </section>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { AppSettings, CpuStatus } from '../models/hardware'
import CircularGauge from './CircularGauge.vue'
import { formatClock, formatPercent, formatTemp, tempLevel, usageLevel } from '../services/format'

const props = defineProps<{
  cpu: CpuStatus
  settings: AppSettings
  showClock: boolean
}>()

const tempCls = computed(() => `temp-${tempLevel(props.cpu.temperature, props.settings)}`)
const usageCls = computed(() => `usage-${usageLevel(props.cpu.usage, props.settings)}`)
const tempColor = computed(() => {
  const level = tempLevel(props.cpu.temperature, props.settings)
  if (level === 'crit') return '#ff7a7a'
  if (level === 'alert') return '#f0a36b'
  if (level === 'warn') return '#e8d27a'
  return '#7dffa3'
})
</script>
