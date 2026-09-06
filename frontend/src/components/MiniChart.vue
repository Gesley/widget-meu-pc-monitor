<template>
  <svg class="mini-chart" viewBox="0 0 160 48" preserveAspectRatio="none">
    <path v-if="area" class="area" :d="area" />
    <path v-if="line" class="line" :d="line" />
  </svg>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  values: Array<number | null>
  max?: number
}>()

const line = computed(() => build(false))
const area = computed(() => build(true))

function build(closed: boolean) {
  const points = props.values.filter((v): v is number => typeof v === 'number')
  if (points.length < 2) return ''
  const max = props.max ?? Math.max(...points, 1)
  const w = 160
  const h = 48
  const step = w / (props.values.length - 1 || 1)
  const coords = props.values.map((v, i) => {
    const yVal = typeof v === 'number' ? v : points[points.length - 1]
    const y = h - (yVal / max) * (h - 4) - 2
    return `${i * step},${y}`
  })
  const d = `M ${coords[0]} L ${coords.slice(1).join(' ')}`
  if (!closed) return d
  return `${d} L ${w},${h} L 0,${h} Z`
}
</script>

<style scoped>
.mini-chart {
  width: 100%;
  height: 42px;
  display: block;
}
.line {
  fill: none;
  stroke: var(--chart);
  stroke-width: 1.6;
  stroke-linejoin: round;
  stroke-linecap: round;
}
.area {
  fill: color-mix(in srgb, var(--chart) 18%, transparent);
}
</style>
