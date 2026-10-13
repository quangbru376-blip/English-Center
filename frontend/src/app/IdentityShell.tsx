import type { ReactNode } from 'react';
import { Brand, LearningIllustration } from '../components/ui/Brand';

export function IdentityShell({ children }: { children: ReactNode }) {
  return (
    <div className="ec-auth">
      <a className="skip-link" href="#main-content">Đến nội dung chính</a>
      <header className="ec-auth-header"><Brand /></header>
      <div className="ec-auth-grid"><aside className="ec-auth-story" aria-label="Giới thiệu trung tâm"><p className="ec-auth-kicker">Không gian học tập của bạn</p><h2>Tiếng Anh,<br/>mở thêm cơ hội.</h2><p>Khóa học, lịch thi và kết quả học tập.<br/>Mỗi bước đều rõ ràng, cùng một nơi.</p><LearningIllustration /></aside>
      <main id="main-content" tabIndex={-1} className="ec-auth-content">
        {children}
      </main>
      </div><footer className="ec-auth-footer">Học tập chủ động · Quản lý rõ ràng</footer>
    </div>
  );
}
