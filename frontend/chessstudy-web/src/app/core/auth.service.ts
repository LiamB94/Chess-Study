import { Injectable, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { tap } from "rxjs/operators";
import { Observable } from "rxjs";

type LoginResponse = {
  accessToken: string;
  user: { userId: number; email: string };
};

type RegisterResponse = {
  accessToken: string;
  user: { userId: number; email: string };
};

@Injectable({ providedIn: 'root' }) export class AuthService {
    private http = inject(HttpClient);
    private tokenKey = 'auth_token';

    login(email: string, password: string) : Observable<LoginResponse> {
        return this.http
            .post<LoginResponse>('/api/auth/login', { email, password })
            .pipe(tap(response => {localStorage.setItem(this.tokenKey, response.accessToken);}));
    }

    logout() : void {
        localStorage.removeItem(this.tokenKey);
    }

    getToken() : string | null {
        return localStorage.getItem(this.tokenKey);
    }

    

    register(email: string, password: string): Observable<RegisterResponse> {
        return this.http
            .post<RegisterResponse>('/api/auth/register', { email, password })
            .pipe(tap(res => localStorage.setItem(this.tokenKey, res.accessToken)));
    }

    getMe() {
        return this.http.get('/api/auth/me');
    }
}
