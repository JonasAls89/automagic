/********************************************************
This is a production server.
To run this: $node server.js
********************************************************/
const express = require('express')
const serveStatic = require('serve-static')
const path = require('path')
const app = express()

// eslint-disable-next-line no-console
console.log('Now running production server on localhost:8080 ...')

app.use('/dist', express.static(path.join(__dirname, 'public'))) // Used to upload everything in the 'public' folder.
app.use(serveStatic('dist', {'index': ['index.html', 'index.htm']}))
app.listen(8080) // This port choice is important in Google Compute Engine
