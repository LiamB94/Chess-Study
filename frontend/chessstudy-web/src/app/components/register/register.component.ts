import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from "@angular/router";
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../core/auth.service';
import { Router } from '@angular/router';



@Component({
  selector: 'app-register',
  imports: [CommonModule, FormsModule, RouterLink],
  standalone: true,
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})

export class RegisterComponent {
  constructor(private auth: AuthService, private router: Router) {}

  error: string | null = null;
  loading = false;
  email = '';
  password = '';
  

  onSubmit() {
  this.error = null;

  if (!this.email || !this.password) {
    this.error = 'Please fill in all fields';
    return;
  }

  this.loading = true;

  this.auth.register(this.email, this.password).subscribe({
    next: () => {
      this.loading = false;
      this.router.navigateByUrl("/login", {state: { registered: true }});
    },
    error: (err: HttpErrorResponse) => {
      const msg =
        (typeof err.error === 'string' ? err.error : err.error?.message) ??
        err.message ??
        'Register failed.';
      this.error = msg;
      this.loading = false;
    }
  });
}

}
