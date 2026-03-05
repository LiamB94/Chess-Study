import { Component, inject, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/auth.service';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from "@angular/router";
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})



export class LoginComponent {
  

  successMessage: string | null = null;
  error: string | null = null;
  loading: boolean = false;
  email: string = '';
  password: string = '';
  registered: boolean = false;

  constructor(private auth: AuthService, private router: Router) {
    // SSR-safe way to read navigation extras state
    const nav = this.router.getCurrentNavigation();
    const registered = (nav?.extras.state as { registered?: boolean } | undefined)?.registered;

    if (registered) {
      this.successMessage = 'Account created! Please log in.';
    }
  }


  onSumbit() {
    this.error = null;

    if (!this.email || !this.password) {
      this.error = 'Please fill in all fields';
      return;
    }

    this.loading = true;

    this.auth.login(this.email, this.password).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigateByUrl("/dashboard")
      },
      error: (err: HttpErrorResponse) => {
        const msg =
          (typeof err.error === 'string' ? err.error : err.error?.message) ??
          err.message ??
          'Login failed.';
        this.error = msg;
        this.loading = false;
        alert("Unluggy");
      }
    });


  }

}
