import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from './header/header';

@Component({
	selector: 'gpp-layout',
	templateUrl: './layout.html',
	styleUrl: './layout.css',
	imports: [RouterOutlet, HeaderComponent],
})
export class LayoutComponent {}
