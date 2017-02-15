import Vue from 'vue'
import Router from 'vue-router'
import ShellView from './../components/Shell/ShellView'

Vue.use(Router)

export default new Router({
  routes: [
    {
      path: '/',
      name: 'Shell',
      component: ShellView
    }
  ]
})
