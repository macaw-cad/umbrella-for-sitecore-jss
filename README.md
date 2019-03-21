# Umbrella for Sitecore JSS


            __.|.__ 
        .n887.d8'qb'""-.
      .d88' .888  q8b. '. 
     d8P'  .8888   .88b. \
    d88_._ d8888_.._9888 _\
      '   '    |    '   '____  ____  ___      ___  _______    _______    _______  ___      ___            __
               |        (""  _||_ ""||"" \    /"" ||   _  ""\/""     \  /""    ""||""|    |"" |         /""""\
               |        |   (  ) : | \   \  //   |(. |_)  :)|:        |(: ______)||  |    ||  |         /    \
               |        (:  |  | . ) /\\  \/.    ||:     \/ |_____/   ) \/    |  |:  |    |:  |        /' /\  \
               |         \\ \__/ // |: \.        |(|  _  \\  //      /  // ___)_  \  |___  \  |___    //  __'  \
               |         /\\ __ //\ |.  \    /:  ||: |_)  :)|:  __   \ (:     ""|( \_|:  \( \_|:  \  /   /  \\  \
             '='        (__________)|___|\__/|___|(_______/ |__|  \___) \_______) \_______)\_______)(___/    \___)
  


Umbrella for Sitecore JSS is a utility that synchronizes items from your Sitecore JSS website to a folder on your computer where you can work directly in your local JSS application even when you're offline. It pulls the current state of your app in Sitecore to your local state. It serves client and server side out of the box and you can deploy straight to docker in Azure. All can be done connected and disconnected.

**Why?**  In Sitecore JSS you can choose between two developer workflow states. I think `Sitecore First` is the one to go with, you cannot start without the `Code First` approach. 

![Umbrella](https://img.shields.io/badge/Umbrella-%20Sitecore%20JSS-red.svg)
[![Build Status MASTER](https://dev.azure.com/MacawInteractive/umbrella-for-sitecore-jss/_apis/build/status/Build%20Master?branchName=master)](https://dev.azure.com/MacawInteractive/umbrella-for-sitecore-jss/_build/latest?definitionId=4&branchName=master)
![downloads](https://img.shields.io/github/downloads/macaw-interactive/umbrella-for-sitecore-jss/total.svg)
![issues](https://img.shields.io/github/issues/macaw-interactive/umbrella-for-sitecore-jss.svg)
![issues](https://img.shields.io/github/watchers/macaw-interactive/umbrella-for-sitecore-jss.svg)
![issues](https://img.shields.io/github/stars/macaw-interactive/umbrella-for-sitecore-jss.svg)
![issues](https://img.shields.io/github/forks/macaw-interactive/umbrella-for-sitecore-jss.svg)

## Table of Contents

- [Umbrella for Sitecore JSS](#umbrella-for-sitecore-jss)
  - [Table of Contents](#table-of-contents)
  - [Dependency](#dependency)
    - [Umbrella.PanTau](#umbrellapantau)
      - [Example output:](#example-output)
  - [Umbrella Sync Script](#umbrella-sync-script)
    - [Usage](#usage)
    - [Options](#options)
  - [Installation](#installation)
  - [API](#api)
    - [Placeholders](#placeholders)
    - [Templates](#templates)
    - [Components](#components)
    - [Content and Media](#content-and-media)
  - [License](#license)


## Dependency

This project can only be used with Sitecore 9.1 with Sitecore JSS installed and is part of our [JSS starter (React+TypeScript)][react-jss-typescript-starter]

### Umbrella.PanTau

The Umbrella.PanTau project adds an extra JSS endpoint: `/sitecore/api/layout/render/umbrella` to your environment. This endpoint is used by the [Umbrella Sync Script][umbrellascript] to extract data from your Sitecore JSS environment.

#### Example output:

```json
{
  "sitecore": {
    "context": {
      "visitorIdentificationTimestamp": 636889392519235300,
      "pageEditing": false,
      "site": {
        "name": "react-jss-typescript-starter"
      },
      "pageState": "normal",
      "language": "en",
      "placeholders": [
        {
          "id": "4cd956d4-a36e-5c78-ab14-3b56f66cb503",
          "name": "footer-address",
          "displayName": "footer-address"
        },
        {
          "id": "c23b835f-a290-5b95-b5d5-e2dbf3ca499e",
          "name": "jss-main",
          "displayName": "Main"
        }
      ],
      "templates": [
        {
          "BaseIDs": [
            "b36ba9fd-0dc0-49c8-bea2-e55d70e6af28"
          ],
          "CustomValues": null,
          "FullName": "Project/react-jss-typescript-starter/App Route",
          "Icon": "Apps/16x16/routes.png",
          "ID": "4e63ea9e-0174-505b-9e6e-82136191959e",
          "Name": "App Route",
          "StandardValueHolderId": "4c235921-3a2f-5f3e-bb91-fc62415c88cf",
          "fields": [
            {
              "id": "d10dc31e-cf38-567f-98d8-655fee342a54",
              "name": "pageTitle",
              "defaultValue": "",
              "type": "Single-Line Text",
              "typeKey": "single-line text",
              "icon": "",
              "isShared": false,
              "inherited": false,
              "templateId": "4e63ea9e-0174-505b-9e6e-82136191959e",
              "templateName": "App Route"
            }
          ]
        }
      ],
      "renderings": [
        {
          "id": "08f9daac-db92-575a-873b-b53cce7de290",
          "name": "ContentBlock",
          "displayName": "Content Block",
          "icon": "Office/16x16/document_tag.png",
          "fields": [
            {
              "id": "037fe404-dd19-4bf7-8e30-4dadf68b27b0",
              "name": "componentName",
              "type": "CommonFieldTypes.SingleLineText"
            }
          ],
          "placeholders": [
            "{C23B835F-A290-5B95-B5D5-E2DBF3CA499E}|jss-main",
            "{11448A04-F955-5B19-A54A-9E0EC07641AF}|jss-reuse-example"
          ]
        }
      ]
    },
    "route": {
      "name": "styleguide",
      "displayName": "styleguide",
      "fields": {
        "pageTitle": {
          "value": "Styleguide | Sitecore JSS"
        }
      },
      "databaseName": "master",
      "deviceId": "fe5d7fdf-89c0-4d99-9aa3-b5fbd009c9f3",
      "itemId": "ed4692a0-7439-59c4-8451-7a55c061158c",
      "itemLanguage": "en",
      "itemVersion": 1,
      "layoutId": "1e09f9fe-4092-5183-9bd2-6a75c1815c59",
      "templateId": "4e63ea9e-0174-505b-9e6e-82136191959e",
      "templateName": "App Route",
      "placeholders": {
        "jss-main": [
        ]
      }
    }
  }
}
```

## Umbrella Sync Script

The Umbrella Sync Script is a NodeJS script and has to be executed from the root of your JSS project folder:

```bash
node .\scripts\umbrella.js
```

### Usage

```bash
node .\scripts\umbrella.js sync
```

### Options

| Switch | Action | 
| --- | --- |
|-t, --templates     | Sync all the templates from Sitecore                 |
|-p, --placeholders  | Sync all the placeholders from Sitecore              |
|-m, --manifests     | Sync all the component manifests from Sitecore       |
|-c, --content       | Sync all the content from your Sitecore JSS website  |

## Installation

> Documentation and sources may change

```bash
git clone https://github.com/macaw-interactive/umbrella-for-sitecore-jss
```

## API

The Umbrella endpoint will output extra data used by the Umbrella NodeJS script. The Umbrella NodeJS script will export data to several sections in your local JSS application.

### Placeholders

Exports all available placeholders from your JSS Sitecore environment to `<app root>/sitecore/definitions/placeholders.sitecore.js`.

### Templates

Exports all available templates from your JSS Sitecore environment to `<app root>/sitecore/definitions/templates`.

### Components

Exports all available components from your JSS Sitecore environment to `<app root>/sitecore/definitions/components`.

### Content and Media

This will export the content of your routes (pages) to your local JSS development environment and saves it in the `<app root>/data/routes` folder. During the export the script will check for image fields and wil save them to the `<app root>/data/media` folder.

## License

[MIT][license] © [Gary Wenneker][author]

[license]: license
[author]: https://gary.wenneker.org
[react-jss-typescript-starter]: https://github.com/macaw-interactive/react-jss-typescript-starter
[umbrellascript]: https://github.com/macaw-interactive/react-jss-typescript-starter/blob/develop/scripts/umbrella.js