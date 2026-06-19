export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  displayName: string;
  expiresAt: string;
}

export interface CurrentUser {
  email: string;
  displayName: string;
  token: string;
  expiresAt: Date;
}
