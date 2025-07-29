import { AuthenticationService } from '@/data/authenticationService';
import { useUserStore } from '@/store/userStore';
import {createRouter, createWebHistory} from 'vue-router';
import { routes } from 'vue-router/auto-routes'
import { setupLayouts } from 'virtual:generated-layouts'

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes: setupLayouts(routes)
})

//Maybe have a list of middlewares and go through them in order. For now, just check if the user is logged in. Its not that many lines of code.
router.beforeEach(async (to, from, next) => {

  let store = useUserStore();
  let user = store.user;

  //dont let people go back to the splash page
  if (to.path === "/" && from.path !== "/") {
    next(from);
    return;
  }
  //if the user is not logged in, redirect them to the welcome page
  const requiresAuth =
    to.meta.requiresAuth !== undefined && to.meta.requiresAuth === false;
  if (!requiresAuth && user === undefined) {
    const refreshToken = localStorage.getItem("refreshToken") as string;
    const userId = localStorage.getItem("userId") as string;
    if (!refreshToken) {
      next("/auth");
      return;
    }

    //i think store gets reset when you switch between apps. So we need to rehydrate the store
    const success = await AuthenticationService.refreshAccessToken(
      refreshToken,
      userId
    );

    if (!success) {
      next("/");
      return;
    }
  }

  user = store.user;

  if (user === undefined) {
    const refreshToken = localStorage.getItem("refreshToken") as string;
    const userId = localStorage.getItem("userId") as string;

    if (refreshToken && userId) {
      await AuthenticationService.refreshAccessToken(refreshToken, userId);
    }

    store = useUserStore();
    user = store.user;
  }

  const userRoles = user
    ?.filter((claim) => claim.type === "role")
    .map((claim) => claim.value);
  if (
    user !== undefined &&
    to.meta.roles &&
    (to.meta.roles as [""]).length > 0 &&
    !(to.meta.roles as [""])?.some((role) => userRoles?.includes(role))
  ) {
    next("/");
    return;
  }

  if (
    to.path === "/" &&
    userRoles &&
    userRoles.length > 0 &&
    userRoles.includes("Admin")
  ) {
    next("/admin");
    return;
  }

  next();
});
export default router;