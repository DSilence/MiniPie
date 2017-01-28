import { connect } from 'react-redux'

import ShellView from '../components/ShellView'

const mapDispatchToProps = {
  //increment : () => increment(1),
  //doubleAsync
}

const mapStateToProps = (state) => ({
  imageSource: state.imageSource
  //counter : state.counter
})

export default connect(mapStateToProps, mapDispatchToProps)(ShellView)