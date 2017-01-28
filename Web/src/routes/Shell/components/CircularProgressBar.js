var ProgressBar = require('progressbar.js')
import React, { Component, PropTypes } from 'react'

//var line = new ProgressBar.Line("#container");

class CircularProgressBar extends Component {
  componentDidMount() {
    var bar = new ProgressBar.Circle('#container', {
      strokeWidth: 6,
      easing: 'easeInOut',
      duration: 1400,
      color: '#FFEA82',
      trailColor: '#eee',
      trailWidth: 1,
      easing: 'easeInOut'
    });
    bar.animate(1.0);
  }
  componentDidUpdate() {

  }
  render() {
    return (
      <div id="container">
      </div>
    );
  }
}

export default CircularProgressBar