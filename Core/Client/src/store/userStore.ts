import { defineStore } from "pinia";
import type { UserClaim } from "@/types/userClaim";

export const useUserStore = defineStore('user', {
    state: () => {
        return {
            user: undefined as UserClaim[] | undefined,
            refreshToken: undefined as string | undefined,
            accessToken: undefined as string | undefined,
            name: undefined as string | undefined,
            profilePicture: undefined as string | undefined,
            notificationCount: 0
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
        setName(name: string) {
            this.name = name;
        },
        setProfilePicture(profilePicture: string) {
            this.profilePicture = profilePicture;
        },
        setNotificationCount(count: number) {
            this.notificationCount = count;
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
        unsetName() {
            this.name = undefined;
        },
        unsetProfilePicture() {
            this.profilePicture = undefined;
        },
        resetNotificationCount() {
            this.notificationCount = 0;
        }
    }
})