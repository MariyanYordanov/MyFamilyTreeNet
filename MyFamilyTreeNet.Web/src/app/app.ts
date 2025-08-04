import { Component, signal } from '@angular/core';
import { MainLayoutComponent } from './layouts/main/main-layout/main-layout';

@Component({
  selector: 'app-root',
  imports: [MainLayoutComponent], 
  template: '<app-main-layout></app-main-layout>' 
})
export class App {
  protected readonly title = signal('MyFamilyTreeNet.Web');
}
