import CircularProgressBar from '../components/CircularProgressBar'
import React, {PropTypes} from 'react'
import './ShellView.scss'

export const ShellView = (props) => (
  <div id="shellView">
    <div id="imageControl">
      <img src={props.imageSource}/>
    </div>
     <CircularProgressBar />
  </div>
)

ShellView.propTypes = {
  imageSource: Prop
}

export default ShellView