export type AuthenticationResponse = {
  incorrectCredentials: boolean;
  twoFactorRequired: boolean;
  /** Set whenever incorrectCredentials is true; a message suitable to show the user directly. */
  errorMessage?: string;
};
