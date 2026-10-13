// Test-only UI navigation adapter. Only opens controls a user can open.
// It never calls auth/business APIs or relaxes response/SQL assertions.
const installed = new WeakSet(), observedContexts = new WeakSet();
function installShellControls(page) {
  if (installed.has(page)) return;
  installed.add(page);
  const context = page.context();
  if (!observedContexts.has(context)) { observedContexts.add(context); context.on('page', installShellControls); }
  const getByRole = page.getByRole.bind(page);
  const accountLabels = new Set(['Đăng xuất', 'Đổi mật khẩu', 'Thông tin cá nhân']);
  const navigationLabels = new Set(['Trang chủ','Chọn khóa học','Lịch thi','Lớp của tôi','Hóa đơn của tôi','Kết quả của tôi','Tài khoản','Nhật ký kiểm toán','Ca thi đầu vào','Khóa học và Band','Chương trình đào tạo','Lớp và lịch học','Duyệt bảng điểm','Lớp và lịch dạy','Kiểm tra đầu vào','Hồ sơ tuyển sinh','Hóa đơn và thanh toán','Báo cáo tài chính','Báo cáo tổng hợp']);
  page.getByRole = (role, options = {}) => {
    // The old duplicated home entry was consolidated into Trang chủ.
    if (role === 'button' && options.name === 'Hồ sơ và lịch thi') options = { ...options, name: 'Trang chủ' };
    const locator = getByRole(role, options), name = options.name;
    if (role !== 'button' || typeof name !== 'string' || (!accountLabels.has(name) && !navigationLabels.has(name))) return locator;
    async function reveal() {
      if (await locator.isVisible()) return;
      const toggle = getByRole('button', { name: accountLabels.has(name) ? 'Menu tài khoản' : 'Mở menu', exact: true });
      if (accountLabels.has(name)) { await toggle.waitFor(); if (await toggle.getAttribute('aria-expanded') === 'false') await toggle.click(); }
      else if (await toggle.isVisible() && await toggle.getAttribute('aria-expanded') === 'false') await toggle.click();
    }
    return new Proxy(locator, { get(target, property) {
      if (property === 'click' || property === 'waitFor') return async (...args) => { await reveal(); return target[property](...args); };
      const value = target[property]; return typeof value === 'function' ? value.bind(target) : value;
    } });
  };
}
module.exports = { installShellControls };
