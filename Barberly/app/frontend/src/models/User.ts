export type User = {
  id: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  userName: string;
  dateOfBirth: string;
  phoneNumber: string;
};

export type LoginUser = {
  username: string;
  password: string;
};
