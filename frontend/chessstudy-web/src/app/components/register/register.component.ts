import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from "@angular/router";
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../core/auth.service';



@Component({
  selector: 'app-register',
  imports: [CommonModule, FormsModule, RouterLink],
  standalone: true,
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})

export class RegisterComponent {
  private auth = inject(AuthService);

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
      console.log('Registered!');
      // later: navigate somewhere
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
