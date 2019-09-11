import Vue from 'vue'
import App from './App'
import VueRouter from 'vue-router'
import Speed from './components/Speed'

Vue.config.productionTip = false
Vue.use(VueRouter)

const routes = [
  { path: '/api/v2/job/create', component: Speed },
  { path: '/api/v2/tables', component: Speed },
  { path: '/api/v2/pipes', component: Speed }
]

const router = new VueRouter({
  routes,
  mode : 'history'
})

new Vue({
  el: '#app',
  router,
  render: h => h(App)
});