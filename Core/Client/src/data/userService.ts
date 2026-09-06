import { useUserStore } from "@/store/userStore";
import type { UserClaim } from "@/types/userClaim";

export class UserService {
    static async GetUserClaims(): Promise<boolean> {
        const response = await fetch("/api/user/claims", {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
                Authorization: `Bearer ${sessionStorage.getItem("accessToken")}`,
            },
        });

        if (!response.ok) {
            return false;
        }
        const userClaims = (await response.json()) as UserClaim[];

        const store = useUserStore();
        store.setUser(userClaims);
        store.setName(
            userClaims.find((claim) => claim.type === "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.value || "",
        );
        return true;
    }
}