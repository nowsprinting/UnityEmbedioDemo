# Unity runtime file server demo using EmbedIO

## Overview

A demo app that starts a web server at runtime and reads and writes `Application.persistentDataPath` directory from an external web browser.

## Requirements

- Unity 6000.0 or later
- [EmbedIO](https://www.nuget.org/packages/embedio/) package v3.5.2
- [HttpMultipartParser](https://www.nuget.org/packages/HttpMultipartParser/) package v9.1.0
- Add `NSLocalNetworkUsageDescription` key to `Info.plist` on iOS14 or later
