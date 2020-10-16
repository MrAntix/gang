import { Config } from '@stencil/core';

// https://stenciljs.com/docs/config

export const config: Config = {
  globalStyle: 'src/global/app.css',
  globalScript: 'src/global/app.ts',
  taskQueue: 'async',
  outputTargets: [
    {
      type: 'www',
      dir: 'wwwroot',
      serviceWorker: {
        globPatterns: [
          '**/*.{js,css,json,html,ico,png}'
        ]
      }
    }
  ],
  testing: {
    timers: 'fake'
  }
};
