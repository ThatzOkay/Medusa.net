import { useUserStore } from "@/store/userStore";
import type { AuthenticationResponse } from "@/types/authenticationResponse";
import type { LoginRequest } from "@/types/loginRequest";
import type { TokenResponse } from "@/types/tokenResponse";
import { UserService } from "./userService";

export class AuthenticationService {
  
  static setAccessToken(accessToken: string): void {
    const store = useUserStore();
    store.setAccessToken(accessToken);
  }

  static async refreshAccessToken(refreshToken: string) {
    const store = useUserStore();
    const response = await fetch("/api/auth/refresh", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        refreshToken: refreshToken
      }),
    });

    if (!response.ok) {
      return false;
    }
    const tokenResponse = (await response.json()) as TokenResponse;

    sessionStorage.setItem("accessToken", tokenResponse.accessToken);
    localStorage.setItem("refreshToken", tokenResponse.refreshToken);

    store.setAccessToken(tokenResponse.accessToken);
    store.setRefreshToken(tokenResponse.refreshToken);
    store.setLoggedIn(true);

    if (await UserService.GetUserClaims()) {
      return true;
    }

    return false;
  }

  static async login(
    loginRequest: LoginRequest,
  ): Promise<AuthenticationResponse> {
    const response = await fetch("/api/auth/login", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(loginRequest),
    });

    if (!response.ok) {
        const json = await response.json();
      if (json.toString().includes(
          "Invalid email or password",
        )
      ) {
        return {
          twoFactorRequired: false,
          incorrectCredentials: true,
        };
      }
    }

    const tokenResponse = (await response.json()) as TokenResponse;

    sessionStorage.setItem("accessToken", tokenResponse.accessToken);
    localStorage.setItem("refreshToken", tokenResponse.refreshToken);

    const store = useUserStore();

    store.setAccessToken(tokenResponse.accessToken);
    store.setRefreshToken(tokenResponse.refreshToken);
    store.setLoggedIn(true);

    if (await UserService.GetUserClaims()) {
      return {
        twoFactorRequired: false,
        incorrectCredentials: false,
      };
    }

    return {
      twoFactorRequired: true,
      incorrectCredentials: true,
    };
  }

  static async registerUser(
    konamiId: string,
    pinCode: string,
    username: string,
    email: string,
    password: string,
  ) {
    const response = await fetch("/api/auth/register", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        konamiId,
        pin: pinCode,
        username,
        email,
        password,
      }),
    });

    if (!response.ok) {
      return false;
    }

    return !(
      await this.login({
        email: email,
        password: password,
        twoFactorCode: undefined,
        twoFactorRecoveryCode: undefined,
      })
    ).incorrectCredentials;
  }
}
