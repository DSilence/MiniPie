import { injectReducer } from '../../store/reducers'

// Sync route definition
export default (store) => ({
  path : 'shell',
  /*  Async getComponent is only invoked when route matches   */
  getComponent (nextState, cb) {
    /*  Webpack - use 'require.ensure' to create a split point
        and embed an async module loader (jsonp) when bundling   */
    require.ensure([], (require) => {
      /*  Webpack - use require callback to define
          dependencies for bundling   */
      const Shell = require('./containers/ShellContainer').default
      const reducer = require('./modules/shell').default

      /*  Add the reducer to the store on key 'counter'  */
      injectReducer(store, { key: 'shell', reducer })

      /*  Return getComponent   */
      cb(null, Shell)

    /* Webpack named bundle   */
    }, 'shell')
  }
})
