<template>
  <section class="ram-card">
    <h3>RAM</h3>
    <div class="ram-row">
      <div class="ram-value">{{ formatGb(ram.used) }} / {{ formatGb(ram.total) }}</div>
      <div :class="usageCls">{{ formatPercent(ram.usage) }}</div>
    </div>
    <div class="bar">
      <span :style="{ width: barWidth }" />
    </div>
    <div class="ram-temp">Temperatura: {{ ram.temperature == null ? 'N/A' : formatTemp(ram.temperature) }}</div>
  </section>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { AppSettings, RamStatus } from '../models/hardware'
import { formatGb, formatPercent, formatTemp, usageLevel } from '../services/format'

const props = defineProps<{
  ram: RamStatus
  settings: AppSettings
}>()

const usageCls = computed(() => `usage-${usageLevel(props.ram.usage, props.settings)}`)
const barWidth = computed(() => `${Math.min(100, Math.max(0, props.ram.usage ?? 0))}%`)
</script>
