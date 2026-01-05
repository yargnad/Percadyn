import { defineConfig } from 'vite'
import fable from 'vite-plugin-fable'

export default defineConfig({
    plugins: [fable({ fsproj: "./src/App.fsproj" })],
})
