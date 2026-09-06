<template>
  <header class="header">
    <div class="brand">PC MONITOR</div>
    <div class="header-actions">
      <button class="icon-btn" type="button" title="Configurações" @click.stop="$emit('open-settings')">⚙</button>
    </div>
  </header>
</template>

<script setup lang="ts">
defineEmits<{ 'open-settings': [] }>()
</script>
