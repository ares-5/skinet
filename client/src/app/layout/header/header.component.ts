import { Component } from '@angular/core';
import {MatBadge} from '@angular/material/badge';
import {MatIcon} from '@angular/material/icon';
import {MatButton} from '@angular/material/button';
import {NgOptimizedImage} from '@angular/common';

@Component({
  selector: 'app-header',
  imports: [
    MatBadge,
    MatIcon,
    MatButton,
    NgOptimizedImage
  ],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent {

}
