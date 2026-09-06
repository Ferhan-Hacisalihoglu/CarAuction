export interface User {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  roleId?: number;
  roleName?: string;
  isActive?: boolean;
  createdAt?: string;
}

export interface Role {
  id: number;
  name: string;
}

export interface Permission {
  id: number;
  name: string;
  description: string;
}

export interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
}

export interface UpdateUserStatusRequest {
  isActive: boolean;
}

export interface UpdateUserRoleRequest {
  roleId: number;
}
