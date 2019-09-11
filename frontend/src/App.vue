<template>
  <div id="app">
    <Speed/>
  </div>
</template>

<script>
import Speed from './components/Speed.vue'

export default {
  name: 'app',
  components: {
    Speed
  }
}
</script>

<style>
#app {
  font-family: 'Avenir', Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}
</style>
