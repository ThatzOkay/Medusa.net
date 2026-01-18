import { defineStore } from "pinia";
import type { UserClaim } from "@/types/userClaim";

export const useUserStore = defineStore("user", {
  state: () => {
    return {
      user: undefined as UserClaim[] | undefined,
      refreshToken: undefined as string | undefined,
      accessToken: undefined as string | undefined,
      isLoggedIn: false,
      name: undefined as string | undefined,
    };
  },
  actions: {
    setUser(user: UserClaim[]) {
      this.user = user;
    },
    setRefreshToken(refreshToken: string) {
      this.refreshToken = refreshToken;
    },
    setAccessToken(accessToken: string) {
      this.accessToken = accessToken;
    },
    setLoggedIn(isLoggedIn: boolean) {
      this.isLoggedIn = isLoggedIn;
    },
    setName(name: string) {
      this.name = name;
    },
    unsetUser() {
      this.user = undefined;
    },
    unsetRefreshToken() {
      this.refreshToken = undefined;
    },
    unsetAccessToken() {
      this.accessToken = undefined;
    },
    unsetLoggedIn() {
      this.isLoggedIn = false;
    },
    unsetName() {
      this.name = undefined;
    },
  },
});
