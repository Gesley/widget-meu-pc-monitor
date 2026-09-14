<template>
  <aside class="settings">
    <h2>Configurações</h2>

    <h3>Aparência</h3>
    <label>
      Opacidade
      <input v-model.number="settings.opacity" type="range" min="0.6" max="1" step="0.1" @change="persist" />
    </label>
    <label>
      Sempre no topo
      <input v-model="settings.alwaysOnTop" type="checkbox" @change="persist" />
    </label>
    <label>
      Iniciar com Windows
      <input v-model="settings.startWithWindows" type="checkbox" @change="persist" />
    </label>
    <label>
      Mostrar gráficos
      <input v-model="settings.showCharts" type="checkbox" @change="persist" />
    </label>
    <label>
      Mostrar clock
      <input v-model="settings.showClock" type="checkbox" @change="persist" />
    </label>
    <label>
      Mostrar RAM
      <input v-model="settings.showRam" type="checkbox" @change="persist" />
    </label>
    <label>
      Modo interação
      <input v-model="settings.interactionMode" type="checkbox" @change="persist" />
    </label>

    <h3>Intervalo</h3>
    <label>
      Polling
      <select v-model.number="settings.pollIntervalMs" @change="persist">
        <option :value="500">500 ms</option>
        <option :value="1000">1 segundo</option>
        <option :value="2000">2 segundos</option>
        <option :value="5000">5 segundos</option>
      </select>
    </label>

    <h3>Layout</h3>
    <label>
      Densidade
      <select v-model="settings.layout" @change="persist">
        <option value="compact">Compacto</option>
        <option value="normal">Normal</option>
      </select>
    </label>

    <h3>Tema</h3>
    <label>
      Tema
      <select v-model="settings.theme" @change="persist">
        <option value="dark">Dark</option>
        <option value="light">Light</option>
        <option value="amoled">AMOLED</option>
      </select>
    </label>

    <h3 v-if="gpus.length">GPU</h3>
    <label v-if="gpus.length">
      GPU principal
      <select v-model="settings.selectedGpuId" @change="persist">
        <option :value="null">Automática</option>
        <option v-for="gpu in gpus" :key="gpu.identifier" :value="gpu.identifier">{{ gpu.name }}</option>
      </select>
    </label>

    <button class="icon-btn" type="button" style="width:auto;padding:0 12px;margin-top:12px" @click.stop="close">
      Fechar
    </button>
  </aside>
</template>

<script setup lang="ts">
import type { AppSettings, GpuOption } from '../models/hardware'

defineProps<{
  settings: AppSettings
  gpus: GpuOption[]
}>()

const emit = defineEmits<{
  close: []
  persist: []
}>()

function persist() {
  emit('persist')
}

function close() {
  emit('close')
}
</script>
