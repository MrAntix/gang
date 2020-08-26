# gang

A state sharing algorithm using a websocket relay server in c# on net5.0 and JS client

- [Overview](#overview)
- [Packages](#packages)
- [Demo](#demo)

## Overview

A gang has a set of members running the same code, one member is designated as the host.

All members can issue commands, these are sent to the current host. 
The host executes the command and broadcasts the current state to all members.

Should the host be disconnected from the gang, an other becomes the host. 

Gangs are controlled by a single relay server, but the work is done by the current host.

## Packages

| Library           | Description                   | Url                                               |
| :---------------- | :-----------------------------| :------------------------------------------------ |
| Gang              | Core library for relay server | https://www.nuget.org/packages/Gang               |
| Gang.WebSockets   | Gang for Web Sockets          | https://www.nuget.org/packages/Gang.WebSockets    |
| @gang-js/core     | JS client service             | https://www.npmjs.com/package/@gang-js/core       |

## Demo

This is a simple Gang Chat app, written with TypeScript and .Net5.0 

Clone the repo and open the solution $/demo/gang.sln
Set Gang.Web as startup

Make sure you build the client app, ```npm install``` then ```npm run build```

Run in visual studio and the ui will popup up.
Open as many browsers as you want on that url to see the gang in action

Also available at https://gang.antix.co.uk 
