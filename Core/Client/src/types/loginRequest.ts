export type LoginRequest = {
  email: string;
  password: string;
  twoFactorCode: string | null | undefined;
  twoFactorRecoveryCode: string | null | undefined;
};
