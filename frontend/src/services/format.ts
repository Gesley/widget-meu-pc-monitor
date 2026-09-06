import type { AppSettings } from '../models/hardware'

export function formatTemp(value: number | null) {
  return value == null ? 'N/A' : `${Math.round(value)}°`
}

export function formatPercent(value: number | null) {
  return value == null ? 'N/A' : `${Math.round(value)}%`
}

export function formatClock(value: number | null) {
  return value == null ? 'N/A' : `${Math.round(value)} MHz`
}

export function formatGb(value: number | null) {
  return value == null ? 'N/A' : `${value.toFixed(1)} GB`
}

export function tempLevel(value: number | null, settings: AppSettings) {
  if (value == null) return 'normal'
  if (value > settings.tempCritical) return 'crit'
  if (value > settings.tempAlert) return 'alert'
  if (value > settings.tempWarn) return 'warn'
  return 'normal'
}

export function usageLevel(value: number | null, settings: AppSettings) {
  if (value == null) return 'normal'
  if (value > settings.usageHigh) return 'high'
  if (value > settings.usageWarn) return 'warn'
  return 'normal'
}
