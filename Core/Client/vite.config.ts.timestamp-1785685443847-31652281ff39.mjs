// vite.config.ts
import { fileURLToPath, URL } from "node:url";
import { defineConfig } from "file:///home/thatzokay/Projects/csharp/Medusa.net/Core/Client/node_modules/vite/dist/node/index.js";
import plugin from "file:///home/thatzokay/Projects/csharp/Medusa.net/Core/Client/node_modules/@vitejs/plugin-vue/dist/index.mjs";
import fs from "fs";
import path from "path";
import child_process from "child_process";
import { env } from "process";
import tailwindcss from "file:///home/thatzokay/Projects/csharp/Medusa.net/Core/Client/node_modules/@tailwindcss/vite/dist/index.mjs";
import Layouts from "file:///home/thatzokay/Projects/csharp/Medusa.net/Core/Client/node_modules/vite-plugin-vue-layouts-next/dist/index.js";
import VueRouter from "file:///home/thatzokay/Projects/csharp/Medusa.net/Core/Client/node_modules/unplugin-vue-router/dist/vite.js";
var __vite_injected_original_import_meta_url = "file:///home/thatzokay/Projects/csharp/Medusa.net/Core/Client/vite.config.ts";
var baseFolder = env.APPDATA !== void 0 && env.APPDATA !== "" ? `${env.APPDATA}/ASP.NET/https` : `${env.HOME}/.aspnet/https`;
fs.mkdirSync(baseFolder, { recursive: true });
var certificateName = "core2.client";
var certFilePath = path.join(baseFolder, `${certificateName}.pem`);
var keyFilePath = path.join(baseFolder, `${certificateName}.key`);
if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
  if (0 !== child_process.spawnSync("dotnet", [
    "dev-certs",
    "https",
    "--export-path",
    certFilePath,
    "--format",
    "Pem",
    "--no-password"
  ], { stdio: "inherit" }).status) {
    throw new Error("Could not create certificate.");
  }
}
var target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` : env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(";")[0] : "https://localhost:7059";
var vite_config_default = defineConfig({
  plugins: [VueRouter({}), tailwindcss(), plugin(), Layouts()],
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", __vite_injected_original_import_meta_url))
    }
  },
  server: {
    proxy: {
      "^/api": {
        target,
        secure: false
      }
    },
    port: 5173,
    https: {
      key: fs.readFileSync(keyFilePath),
      cert: fs.readFileSync(certFilePath)
    }
  }
});
export {
  vite_config_default as default
};
//# sourceMappingURL=data:application/json;base64,ewogICJ2ZXJzaW9uIjogMywKICAic291cmNlcyI6IFsidml0ZS5jb25maWcudHMiXSwKICAic291cmNlc0NvbnRlbnQiOiBbImNvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9kaXJuYW1lID0gXCIvaG9tZS90aGF0em9rYXkvUHJvamVjdHMvY3NoYXJwL01lZHVzYS5uZXQvQ29yZS9DbGllbnRcIjtjb25zdCBfX3ZpdGVfaW5qZWN0ZWRfb3JpZ2luYWxfZmlsZW5hbWUgPSBcIi9ob21lL3RoYXR6b2theS9Qcm9qZWN0cy9jc2hhcnAvTWVkdXNhLm5ldC9Db3JlL0NsaWVudC92aXRlLmNvbmZpZy50c1wiO2NvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9pbXBvcnRfbWV0YV91cmwgPSBcImZpbGU6Ly8vaG9tZS90aGF0em9rYXkvUHJvamVjdHMvY3NoYXJwL01lZHVzYS5uZXQvQ29yZS9DbGllbnQvdml0ZS5jb25maWcudHNcIjtpbXBvcnQgeyBmaWxlVVJMVG9QYXRoLCBVUkwgfSBmcm9tICdub2RlOnVybCc7XG5pbXBvcnQgeyBkZWZpbmVDb25maWcgfSBmcm9tICd2aXRlJztcbmltcG9ydCBwbHVnaW4gZnJvbSAnQHZpdGVqcy9wbHVnaW4tdnVlJztcbmltcG9ydCBmcyBmcm9tICdmcyc7XG5pbXBvcnQgcGF0aCBmcm9tICdwYXRoJztcbmltcG9ydCBjaGlsZF9wcm9jZXNzIGZyb20gJ2NoaWxkX3Byb2Nlc3MnO1xuaW1wb3J0IHsgZW52IH0gZnJvbSAncHJvY2Vzcyc7XG5pbXBvcnQgdGFpbHdpbmRjc3MgZnJvbSAnQHRhaWx3aW5kY3NzL3ZpdGUnXG5pbXBvcnQgTGF5b3V0cyBmcm9tICd2aXRlLXBsdWdpbi12dWUtbGF5b3V0cy1uZXh0J1xuaW1wb3J0IFZ1ZVJvdXRlciBmcm9tICd1bnBsdWdpbi12dWUtcm91dGVyL3ZpdGUnXG5cbmNvbnN0IGJhc2VGb2xkZXIgPVxuICAgIGVudi5BUFBEQVRBICE9PSB1bmRlZmluZWQgJiYgZW52LkFQUERBVEEgIT09ICcnXG4gICAgICAgID8gYCR7ZW52LkFQUERBVEF9L0FTUC5ORVQvaHR0cHNgXG4gICAgICAgIDogYCR7ZW52LkhPTUV9Ly5hc3BuZXQvaHR0cHNgO1xuXG5mcy5ta2RpclN5bmMoYmFzZUZvbGRlciwgeyByZWN1cnNpdmU6IHRydWUgfSk7XG5cbmNvbnN0IGNlcnRpZmljYXRlTmFtZSA9IFwiY29yZTIuY2xpZW50XCI7XG5jb25zdCBjZXJ0RmlsZVBhdGggPSBwYXRoLmpvaW4oYmFzZUZvbGRlciwgYCR7Y2VydGlmaWNhdGVOYW1lfS5wZW1gKTtcbmNvbnN0IGtleUZpbGVQYXRoID0gcGF0aC5qb2luKGJhc2VGb2xkZXIsIGAke2NlcnRpZmljYXRlTmFtZX0ua2V5YCk7XG5cbmlmICghZnMuZXhpc3RzU3luYyhjZXJ0RmlsZVBhdGgpIHx8ICFmcy5leGlzdHNTeW5jKGtleUZpbGVQYXRoKSkge1xuICAgIGlmICgwICE9PSBjaGlsZF9wcm9jZXNzLnNwYXduU3luYygnZG90bmV0JywgW1xuICAgICAgICAnZGV2LWNlcnRzJyxcbiAgICAgICAgJ2h0dHBzJyxcbiAgICAgICAgJy0tZXhwb3J0LXBhdGgnLFxuICAgICAgICBjZXJ0RmlsZVBhdGgsXG4gICAgICAgICctLWZvcm1hdCcsXG4gICAgICAgICdQZW0nLFxuICAgICAgICAnLS1uby1wYXNzd29yZCcsXG4gICAgXSwgeyBzdGRpbzogJ2luaGVyaXQnLCB9KS5zdGF0dXMpIHtcbiAgICAgICAgdGhyb3cgbmV3IEVycm9yKFwiQ291bGQgbm90IGNyZWF0ZSBjZXJ0aWZpY2F0ZS5cIik7XG4gICAgfVxufVxuXG5jb25zdCB0YXJnZXQgPSBlbnYuQVNQTkVUQ09SRV9IVFRQU19QT1JUID8gYGh0dHBzOi8vbG9jYWxob3N0OiR7ZW52LkFTUE5FVENPUkVfSFRUUFNfUE9SVH1gIDpcbiAgICBlbnYuQVNQTkVUQ09SRV9VUkxTID8gZW52LkFTUE5FVENPUkVfVVJMUy5zcGxpdCgnOycpWzBdIDogJ2h0dHBzOi8vbG9jYWxob3N0OjcwNTknO1xuXG4vLyBodHRwczovL3ZpdGVqcy5kZXYvY29uZmlnL1xuZXhwb3J0IGRlZmF1bHQgZGVmaW5lQ29uZmlnKHtcbiAgICBwbHVnaW5zOiBbVnVlUm91dGVyKHt9KSwgdGFpbHdpbmRjc3MoKSwgcGx1Z2luKCksIExheW91dHMoKV0sXG4gICAgcmVzb2x2ZToge1xuICAgICAgICBhbGlhczoge1xuICAgICAgICAgICAgJ0AnOiBmaWxlVVJMVG9QYXRoKG5ldyBVUkwoJy4vc3JjJywgaW1wb3J0Lm1ldGEudXJsKSlcbiAgICAgICAgfVxuICAgIH0sXG4gICAgc2VydmVyOiB7XG4gICAgICAgIHByb3h5OiB7XG4gICAgICAgICAgICAnXi9hcGknOiB7XG4gICAgICAgICAgICAgICAgdGFyZ2V0LFxuICAgICAgICAgICAgICAgIHNlY3VyZTogZmFsc2VcbiAgICAgICAgICAgIH0sXG4gICAgICAgIH0sXG4gICAgICAgIHBvcnQ6IDUxNzMsXG4gICAgICAgIGh0dHBzOiB7XG4gICAgICAgICAgICBrZXk6IGZzLnJlYWRGaWxlU3luYyhrZXlGaWxlUGF0aCksXG4gICAgICAgICAgICBjZXJ0OiBmcy5yZWFkRmlsZVN5bmMoY2VydEZpbGVQYXRoKSxcbiAgICAgICAgfVxuICAgIH1cbn0pXG4iXSwKICAibWFwcGluZ3MiOiAiO0FBQW9WLFNBQVMsZUFBZSxXQUFXO0FBQ3ZYLFNBQVMsb0JBQW9CO0FBQzdCLE9BQU8sWUFBWTtBQUNuQixPQUFPLFFBQVE7QUFDZixPQUFPLFVBQVU7QUFDakIsT0FBTyxtQkFBbUI7QUFDMUIsU0FBUyxXQUFXO0FBQ3BCLE9BQU8saUJBQWlCO0FBQ3hCLE9BQU8sYUFBYTtBQUNwQixPQUFPLGVBQWU7QUFUOEwsSUFBTSwyQ0FBMkM7QUFXclEsSUFBTSxhQUNGLElBQUksWUFBWSxVQUFhLElBQUksWUFBWSxLQUN2QyxHQUFHLElBQUksT0FBTyxtQkFDZCxHQUFHLElBQUksSUFBSTtBQUVyQixHQUFHLFVBQVUsWUFBWSxFQUFFLFdBQVcsS0FBSyxDQUFDO0FBRTVDLElBQU0sa0JBQWtCO0FBQ3hCLElBQU0sZUFBZSxLQUFLLEtBQUssWUFBWSxHQUFHLGVBQWUsTUFBTTtBQUNuRSxJQUFNLGNBQWMsS0FBSyxLQUFLLFlBQVksR0FBRyxlQUFlLE1BQU07QUFFbEUsSUFBSSxDQUFDLEdBQUcsV0FBVyxZQUFZLEtBQUssQ0FBQyxHQUFHLFdBQVcsV0FBVyxHQUFHO0FBQzdELE1BQUksTUFBTSxjQUFjLFVBQVUsVUFBVTtBQUFBLElBQ3hDO0FBQUEsSUFDQTtBQUFBLElBQ0E7QUFBQSxJQUNBO0FBQUEsSUFDQTtBQUFBLElBQ0E7QUFBQSxJQUNBO0FBQUEsRUFDSixHQUFHLEVBQUUsT0FBTyxVQUFXLENBQUMsRUFBRSxRQUFRO0FBQzlCLFVBQU0sSUFBSSxNQUFNLCtCQUErQjtBQUFBLEVBQ25EO0FBQ0o7QUFFQSxJQUFNLFNBQVMsSUFBSSx3QkFBd0IscUJBQXFCLElBQUkscUJBQXFCLEtBQ3JGLElBQUksa0JBQWtCLElBQUksZ0JBQWdCLE1BQU0sR0FBRyxFQUFFLENBQUMsSUFBSTtBQUc5RCxJQUFPLHNCQUFRLGFBQWE7QUFBQSxFQUN4QixTQUFTLENBQUMsVUFBVSxDQUFDLENBQUMsR0FBRyxZQUFZLEdBQUcsT0FBTyxHQUFHLFFBQVEsQ0FBQztBQUFBLEVBQzNELFNBQVM7QUFBQSxJQUNMLE9BQU87QUFBQSxNQUNILEtBQUssY0FBYyxJQUFJLElBQUksU0FBUyx3Q0FBZSxDQUFDO0FBQUEsSUFDeEQ7QUFBQSxFQUNKO0FBQUEsRUFDQSxRQUFRO0FBQUEsSUFDSixPQUFPO0FBQUEsTUFDSCxTQUFTO0FBQUEsUUFDTDtBQUFBLFFBQ0EsUUFBUTtBQUFBLE1BQ1o7QUFBQSxJQUNKO0FBQUEsSUFDQSxNQUFNO0FBQUEsSUFDTixPQUFPO0FBQUEsTUFDSCxLQUFLLEdBQUcsYUFBYSxXQUFXO0FBQUEsTUFDaEMsTUFBTSxHQUFHLGFBQWEsWUFBWTtBQUFBLElBQ3RDO0FBQUEsRUFDSjtBQUNKLENBQUM7IiwKICAibmFtZXMiOiBbXQp9Cg==
