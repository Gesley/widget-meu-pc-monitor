<template>
  <svg class="gauge" viewBox="0 0 120 120" aria-hidden="true">
    <circle class="track" cx="60" cy="60" r="46" />
    <circle
      class="arc"
      cx="60"
      cy="60"
      r="46"
      :stroke="arcColor"
      :stroke-dasharray="circumference"
      :stroke-dashoffset="offset"
    />
    <text x="60" y="58" class="value">{{ label }}</text>
    <text x="60" y="76" class="sub">TEMP</text>
  </svg>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  value: number | null
  label: string
  color: string
}>()

const radius = 46
const circumference = 2 * Math.PI * radius
const offset = computed(() => {
  const pct = Math.min(100, Math.max(0, props.value ?? 0)) / 100
  return circumference * (1 - pct)
})
const arcColor = computed(() => props.color)
</script>

<style scoped>
.gauge {
  width: 132px;
  height: 132px;
}
.track,
.arc {
  fill: none;
  stroke-width: 7;
  stroke-linecap: round;
  transform: rotate(-90deg);
  transform-origin: 60px 60px;
}
.track {
  stroke: rgba(255, 255, 255, 0.12);
}
.arc {
  transition: stroke-dashoffset 0.45s ease, stroke 0.45s ease;
}
.value {
  fill: currentColor;
  font-size: 22px;
  font-weight: 600;
  text-anchor: middle;
}
.sub {
  fill: var(--muted);
  font-size: 8px;
  letter-spacing: 0.16em;
  text-anchor: middle;
}
</style>
