import { Component, inject, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/auth.service';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from "@angular/router";

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})

export class LoginComponent {
  private auth = inject(AuthService);

  error: string | null = null;
  loading: boolean = false;
  email: string = '';
  password: string = '';


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
        alert("YOUZZAA IT WORKED");
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
