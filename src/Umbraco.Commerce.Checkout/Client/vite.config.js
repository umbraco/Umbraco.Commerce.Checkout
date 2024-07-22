
import { defineConfig } from 'vite';
import { viteStaticCopy } from 'vite-plugin-static-copy';
import tsconfigPaths from 'vite-tsconfig-paths';

export default defineConfig({
    build: {
        lib: {
            entry: {
                'uccheckout.backoffice': 'src/backoffice/index.ts',
                'uccheckout.surface': 'src/surface/index.ts',
            },
            formats: ['es'],
        },
        outDir: '../wwwroot/',
        emptyOutDir: true,
        sourcemap: true,
        rollupOptions: {
            external: [/^@umbraco/],
            onwarn: () => { },
            output: {
                assetFileNames: (assetInfo) => {
                    if (assetInfo.name === 'uccheckout.css') {
                        return 'uccheckout.surface.css';
                    }

                    return '[name].[ext]';
                },
            },
        },
        cssCodeSplit: true,
    },
    plugins: [
        tsconfigPaths(),
        viteStaticCopy({
            targets: [
                {
                    src: 'src/umbraco-package.json',
                    dest: '../wwwroot/',
                },
            ],
        }),
    ],
});
